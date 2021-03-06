﻿namespace Simple.Web.Behaviors.Implementations
{
    using MediaTypeHandling;
    using Behaviors;
    using Http;

    /// <summary>
    /// This type supports the framework directly and should not be used from your code.
    /// </summary>
    public static class SetInput
    {
        /// <summary>
        /// This method supports the framework directly and should not be used from your code
        /// </summary>
        /// <param name="handler">The handler.</param>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        public static void Impl<T>(IInput<T> handler, IContext context)
        {
        	if (context.Request.InputStream.Length == 0) return;

            var mediaTypeHandlerTable = new MediaTypeHandlerTable();
            var mediaTypeHandler = mediaTypeHandlerTable.GetMediaTypeHandler(context.Request.ContentType);
            handler.Input = (T)mediaTypeHandler.Read(context.Request.InputStream, typeof(T));
        }
    }
}
