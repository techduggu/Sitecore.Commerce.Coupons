using Microsoft.Extensions.DependencyInjection;
using Sitecore.Commerce.Core;
using Sitecore.Commerce.Plugin.Rules;
using Sitecore.Framework.Configuration;
using Sitecore.Framework.Pipelines.Definitions.Extensions;
using Sitecore.Framework.Rules;
using System.Reflection;

namespace Sample.Plugin.Promotions
{
    public class ConfigureSitecore : IConfigureSitecore
    {
        public void ConfigureServices(IServiceCollection services)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            services.RegisterAllPipelineBlocks(assembly);
            services.RegisterAllCommands(assembly);

            //Register this plugin assembly with Sitecore Rules service
            services.Sitecore().Rules(rules => rules.Registry(registry => registry.RegisterAssembly(assembly)));

            //Register our custom and extended BuildRuleSetBlock
            services.Sitecore().Pipelines(config => config
            .ConfigurePipeline<IBuildRuleSetPipeline>(builder => builder
               .Remove<Sitecore.Commerce.Plugin.Rules.BuildRuleSetBlock>()
               .Add<Sample.Plugin.Promotions.Pipelines.Blocks.BuildRuleSetBlock>()
               )
            );
        }
    }
}
