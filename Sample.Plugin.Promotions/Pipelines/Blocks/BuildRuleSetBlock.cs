using Microsoft.Extensions.DependencyInjection;
using Sitecore.Commerce.Core;
using Sitecore.Commerce.Plugin.Rules;
using Sitecore.Framework.Conditions;
using Sitecore.Framework.Pipelines;
using Sitecore.Framework.Rules;
using Sitecore.Framework.Rules.Registry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Sample.Plugin.Promotions.Extensions;

namespace Sample.Plugin.Promotions.Pipelines.Blocks
{
    public class BuildRuleSetBlock : PipelineBlock<IEnumerable<RuleModel>, RuleSet, CommercePipelineExecutionContext>
    {
        private readonly IEntityRegistry _entityRegistry;
        private IRuleBuilderInit _ruleBuilder;
        private readonly IServiceProvider _services;

        public BuildRuleSetBlock(IEntityRegistry entityRegistry, IServiceProvider services)
          : base((string)null)
        {
            this._entityRegistry = entityRegistry;
            this._services = services;
        }

        public override async Task<RuleSet> Run(IEnumerable<RuleModel> arg, CommercePipelineExecutionContext context)
        {
            BuildRuleSetBlock buildRuleSetBlock = this;
            List<RuleModel> ruleModels = arg as List<RuleModel> ?? arg.ToList<RuleModel>();
            // ISSUE: explicit non-virtual call
            Condition.Requires<List<RuleModel>>(ruleModels).IsNotNull<List<RuleModel>>(string.Format("{0}: The argument cannot be null", (object)(buildRuleSetBlock.Name)));
            CommercePipelineExecutionContext executionContext;
            if (!ruleModels.Any<RuleModel>())
            {
                executionContext = context;
                executionContext.Abort(await context.CommerceContext.AddMessage(context.GetPolicy<KnownResultCodes>().Error, "RulesCannotBeNullOrEmpty", (object[])null, "Rules cannot be null or empty."), (object)context);
                executionContext = (CommercePipelineExecutionContext)null;
                return (RuleSet)null;
            }
            buildRuleSetBlock._ruleBuilder = buildRuleSetBlock._services.GetService<IRuleBuilderInit>();
            RuleSet ruleSet1 = new RuleSet();
            ruleSet1.Id = string.Format("{0}{1:N}", (object)CommerceEntity.IdPrefix<RuleSet>(), (object)Guid.NewGuid());
            RuleSet ruleSet = ruleSet1;
            foreach (RuleModel model in ruleModels.Where<RuleModel>((Func<RuleModel, bool>)(rm => rm != null)))
            {
                try
                {
                    ruleSet.Rules.Add(buildRuleSetBlock.BuildRule(model));
                }
                catch (Exception ex)
                {
                    executionContext = context;
                    CommerceContext commerceContext = context.CommerceContext;
                    string error = context.GetPolicy<KnownResultCodes>().Error;
                    string commerceTermKey = "RuleNotBuilt";
                    object[] args = new object[2]
                    {
            (object) model.Name,
            (object) ex
                    };
                    string defaultMessage = string.Format("Rule '{0}' cannot be built.", (object)model.Name);
                    executionContext.Abort(await commerceContext.AddMessage(error, commerceTermKey, args, defaultMessage), (object)context);
                    executionContext = (CommercePipelineExecutionContext)null;
                    return (RuleSet)null;
                }
            }
            return ruleSet;
        }

        protected virtual IRule BuildRule(RuleModel model)
        {
            ConditionModel model1 = model.Conditions.First<ConditionModel>();
            IEntityMetadata metadata1 = this._entityRegistry.GetMetadata(model1.LibraryId);
            IRuleBuilder ruleBuilder = this._ruleBuilder.When(model1.ExtendedConvertToCondition(metadata1, this._entityRegistry, this._services));
            for (int index = 1; index < model.Conditions.Count; ++index)
            {
                ConditionModel condition1 = model.Conditions[index];
                IEntityMetadata metadata2 = this._entityRegistry.GetMetadata(condition1.LibraryId);
                ICondition condition2 = condition1.ExtendedConvertToCondition(metadata2, this._entityRegistry, this._services);
                if (!string.IsNullOrEmpty(condition1.ConditionOperator))
                {
                    if (condition1.ConditionOperator.ToUpper() == "OR")
                        ruleBuilder.Or(condition2);
                    else
                        ruleBuilder.And(condition2);
                }
            }
            foreach (ActionModel thenAction in (IEnumerable<ActionModel>)model.ThenActions)
            {
                IEntityMetadata metadata2 = this._entityRegistry.GetMetadata(thenAction.LibraryId);
                IAction action = thenAction.ExtendedConvertToAction(metadata2, this._entityRegistry, this._services);
                ruleBuilder.Then(action);
            }
            foreach (ActionModel elseAction in (IEnumerable<ActionModel>)model.ElseActions)
            {
                IEntityMetadata metadata2 = this._entityRegistry.GetMetadata(elseAction.LibraryId);
                IAction action = elseAction.ExtendedConvertToAction(metadata2, this._entityRegistry, this._services);
                ruleBuilder.Else(action);
            }
            return ruleBuilder.ToRule();
        }
    }
}
