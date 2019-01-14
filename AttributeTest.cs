namespace SwaggerAttributes
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using CustomSwaggerAttributes;
    using Newtonsoft.Json;
    using Swashbuckle.AspNetCore.Swagger;
    using Swashbuckle.AspNetCore.SwaggerGen;
    using TestFixtures;
    using Xunit;
    
    public class AttributeTest
    {
        protected Operation Operation { get; private set; }
        protected CommentsOperationFilter Filter { get; }

        public AttributeTest()
        {
            Filter = new CommentsOperationFilter();
            Operation = new Operation
            {
                OperationId = "TestOperation",
                Responses = new Dictionary<string, Response>()
            };
        }

        [Fact]
        public void AttributesAreProcessedInOrder()
        { 
            // Arrange
            var operation = new Operation
            {
                OperationId = "DummyOperation",
            };  
            var filterContext = FilterContextFor<AttributeDummyController>("AttributeDummyMethod");
            
            // Act
            new CommentsOperationFilter().Apply(operation, filterContext);
            
            //Assert
            Assert.Equal("Foo\nBar\nBaz", operation.Summary);
        }
        
        protected OperationFilterContext FilterContextFor<T>(string actionFixtureName) where T: class
        {
            var fakeProvider = new FakeApiDescriptionGroupCollectionProvider();
            var apiDescriptions = fakeProvider.Add<T>(actionFixtureName);
            var apiDescription = apiDescriptions.ApiDescriptionGroups.Items.First().Items.First();

            return new OperationFilterContext(
                apiDescription, 
                new SchemaRegistry(new JsonSerializerSettings()), 
                typeof(T).GetMethod(actionFixtureName));
        }
    }
}