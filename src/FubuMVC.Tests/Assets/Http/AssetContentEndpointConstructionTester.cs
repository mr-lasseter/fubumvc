using System.Linq;
using FubuMVC.Core.Assets.Caching;
using FubuMVC.Core.Assets.Files;
using FubuMVC.Core.Assets.Http;
using FubuMVC.Core.Caching;
using FubuMVC.Core.Registration;
using FubuMVC.Core.Registration.Nodes;
using FubuMVC.Core.Resources.PathBased;
using FubuTestingSupport;
using NUnit.Framework;

namespace FubuMVC.Tests.Assets.Http
{
    [TestFixture]
    public class AssetContentEndpointConstructionTester
    {
        #region Setup/Teardown

        [SetUp]
        public void SetUp()
        {
            var behaviorGraph = BehaviorGraph.BuildEmptyGraph();

            theContentCache = behaviorGraph.Services.DefaultServiceFor<IAssetContentCache>()
                .Value.ShouldBeOfType<AssetContentCache>();

            theChain = behaviorGraph.BehaviorFor<AssetWriter>(x => x.Write(null));
        }

        #endregion

        private BehaviorChain theChain;
        private AssetContentCache theContentCache;

        [Test]
        public void generates_url_with_asset_path_of_different_lengths()
        {
            theChain.Route.CreateUrlFromInput(new AssetPath("scripts/file1.js")).ShouldEqual("_content/scripts/file1.js");
            theChain.Route.CreateUrlFromInput(new AssetPath("scripts/f1/file1.js")).ShouldEqual(
                "_content/scripts/f1/file1.js");
            theChain.Route.CreateUrlFromInput(new AssetPath("scripts/f1/f2/file1.js")).ShouldEqual(
                "_content/scripts/f1/f2/file1.js");
            theChain.Route.CreateUrlFromInput(new AssetPath("scripts/f1/f2/f3/file1.js")).ShouldEqual(
                "_content/scripts/f1/f2/f3/file1.js");
        }

        [Test]
        public void route_pattern_should_be_the_same()
        {
            theChain.Route.Pattern.ShouldEqual("_content/" + ResourcePath.UrlSuffix);
        }

        [Test]
        public void same_asset_content_cache_should_be_used_everywhere()
        {
            var cachingNode = theChain.FirstCall().Previous.ShouldBeOfType<OutputCachingNode>();
            cachingNode.ETagCache.Value.ShouldBeTheSameAs(theContentCache);
            cachingNode.OutputCache.Value.ShouldBeTheSameAs(theContentCache);
        }

        [Test]
        public void should_apply_a_caching_node_before_the_action_that_uses_the_asset_content_cache()
        {
            var cachingNode = theChain.FirstCall().Previous.ShouldBeOfType<OutputCachingNode>();
            cachingNode.ETagCache.Value.ShouldBeOfType<AssetContentCache>();
            cachingNode.OutputCache.Value.ShouldBeOfType<AssetContentCache>();
        }

        [Test]
        public void the_chain_exists()
        {
            theChain.ShouldNotBeNull();
        }

        [Test]
        public void the_chain_should_have_an_asset_etag_invocation_filter()
        {
            theChain.Filters.Single().ShouldBeOfType<AssetEtagInvocationFilter>();
        }
    }
}