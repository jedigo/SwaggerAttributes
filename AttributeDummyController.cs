using System;

namespace SwaggerAttributes
{
    using System.Net;
    using CustomSwaggerAttributes;

    public class AttributeDummyController
    {
        [SwaggerComment("Foo")]
        [SwaggerComment("Bar")]
        [SwaggerComment("Baz")]
        public void AttributeDummyMethod()
        {
            return;
        }
    }
}
