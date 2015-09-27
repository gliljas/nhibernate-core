using System;
using System.Collections;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using NHibernate.Linq.ExpressionTransformers;
using Remotion.Linq;
using Remotion.Linq.Clauses;
using Remotion.Linq.Clauses.StreamedData;
using Remotion.Linq.EagerFetching.Parsing;
using Remotion.Linq.Parsing.ExpressionTreeVisitors.Transformation;
using Remotion.Linq.Parsing.Structure;
using Remotion.Linq.Parsing.Structure.IntermediateModel;
using Remotion.Linq.Parsing.Structure.NodeTypeProviders;

namespace NHibernate.Linq
{
	public static class NhRelinqQueryParser
	{
		private static readonly QueryParser QueryParser;

		static NhRelinqQueryParser()
		{
			var transformerRegistry = ExpressionTransformerRegistry.CreateDefault();
			transformerRegistry.Register(new RemoveCharToIntConversion());
			transformerRegistry.Register(new RemoveRedundantCast());
			transformerRegistry.Register(new SimplifyCompareTransformer());

			var processor = ExpressionTreeParser.CreateDefaultProcessor(transformerRegistry);
			// Add custom processors here:
			// processor.InnerProcessors.Add (new MyExpressionTreeProcessor());

			var nodeTypeProvider = new NHibernateNodeTypeProvider();

			var expressionTreeParser = new ExpressionTreeParser(nodeTypeProvider, processor);
			QueryParser = new QueryParser(expressionTreeParser);
		}

		public static QueryModel Parse(Expression expression)
		{
			return QueryParser.GetParsedQuery(expression);
		}
	}

	public class NHibernateNodeTypeProvider : INodeTypeProvider
	{
		private INodeTypeProvider defaultNodeTypeProvider;

		public NHibernateNodeTypeProvider()
		{
			var methodInfoRegistry = new MethodInfoBasedNodeTypeRegistry();

			methodInfoRegistry.Register(new[] { typeof(EagerFetchingExtensionMethods).GetMethod("Fetch") }, typeof(FetchOneExpressionNode));
			methodInfoRegistry.Register(new[] { typeof(EagerFetchingExtensionMethods).GetMethod("FetchMany") }, typeof(FetchManyExpressionNode));
			methodInfoRegistry.Register(new[] { typeof(EagerFetchingExtensionMethods).GetMethod("ThenFetch") }, typeof(ThenFetchOneExpressionNode));
			methodInfoRegistry.Register(new[] { typeof(EagerFetchingExtensionMethods).GetMethod("ThenFetchMany") }, typeof(ThenFetchManyExpressionNode));

			methodInfoRegistry.Register(
				new[]
					{
						ReflectionHelper.GetMethodDefinition(() => Queryable.AsQueryable(null)),
						ReflectionHelper.GetMethodDefinition(() => Queryable.AsQueryable<object>(null)),
					}, typeof(AsQueryableExpressionNode)
				);

			var nodeTypeProvider = ExpressionTreeParser.CreateDefaultNodeTypeProvider();
			nodeTypeProvider.InnerProviders.Add(methodInfoRegistry);
			defaultNodeTypeProvider = nodeTypeProvider;
		}

		public bool IsRegistered(MethodInfo method)
		{
			// Avoid Relinq turning IDictionary.Contains into ContainsResultOperator.  We do our own processing for that method.
			if (method.DeclaringType == typeof(IDictionary) && method.Name == "Contains")
				return false;

			return defaultNodeTypeProvider.IsRegistered(method);
		}

		public System.Type GetNodeType(MethodInfo method)
		{
			return defaultNodeTypeProvider.GetNodeType(method);
		}
	}

	public class AsQueryableExpressionNode : MethodCallExpressionNodeBase
	{
		public AsQueryableExpressionNode(MethodCallExpressionParseInfo parseInfo) : base(parseInfo)
		{
		}

		public override Expression Resolve(ParameterExpression inputParameter, Expression expressionToBeResolved, ClauseGenerationContext clauseGenerationContext)
		{
			return Source.Resolve(inputParameter, expressionToBeResolved, clauseGenerationContext);
		}

		protected override QueryModel ApplyNodeSpecificSemantics(QueryModel queryModel, ClauseGenerationContext clauseGenerationContext)
		{
			return queryModel;
		}
	}
	
}
