namespace Dashing.Tests.CodeGeneration {
    using Dashing.CodeGeneration;
    using Dashing.Tests.CodeGeneration.Fixtures;
    using Dashing.Tests.TestDomain;

    using Xunit;

    public class UpdateTests : IClassFixture<GenerateCodeFixture> {
        private readonly IGeneratedCodeManager codeManager;

        public UpdateTests(GenerateCodeFixture data) {
            this.codeManager = data.CodeManager;
        }

        [Fact]
        public void CreateUpdateInstanceDoesntExplodeWithConstructorInitializedProperties() {
            // act
            var updatePost = this.codeManager.CreateUpdateInstance<Post>();

            // assert
            Assert.NotNull(updatePost);
        }

        [Fact]
        public void PropertiesInitializedInConstructorAreNotMarkedAsUpdated() {
            // act
            var updatePost = this.codeManager.CreateUpdateInstance<Post>();

            // assert
            // ReSharper disable once SuspiciousTypeConversion.Global Reviewed, ok here.
            var updateClass = updatePost as IUpdateClass;
            Assert.NotNull(updateClass);
            Assert.Empty(updateClass.UpdatedProperties);
        }
    }
}