namespace SwaggerAttributes.CustomSwaggerAttributes
{
    using System;

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class SwaggerCommentAttribute : Attribute
    {
        public SwaggerCommentAttribute(string comment)
        {
            Comment = comment;
        }

        public string Comment { get; }
    }
}