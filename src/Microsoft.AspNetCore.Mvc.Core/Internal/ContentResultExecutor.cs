﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Buffers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Microsoft.AspNetCore.Mvc.Internal
{
    public class ContentResultExecutor
    {
        private const string DefaultContentType = "text/plain; charset=utf-8";
        private readonly ILogger<ContentResultExecutor> _logger;
        private readonly IHttpResponseStreamWriterFactory _httpResponseStreamWriterFactory;

        public ContentResultExecutor(ILogger<ContentResultExecutor> logger, IHttpResponseStreamWriterFactory httpResponseStreamWriterFactory)
        {
            _logger = logger;
            _httpResponseStreamWriterFactory = httpResponseStreamWriterFactory;
        }

        public async Task ExecuteAsync(ActionContext context, ContentResult result)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (result == null)
            {
                throw new ArgumentNullException(nameof(result));
            }

            var response = context.HttpContext.Response;

            string resolvedContentType;
            Encoding resolvedContentTypeEncoding;
            ResponseContentTypeHelper.ResolveContentTypeAndEncoding(
                result.ContentType,
                response.ContentType,
                DefaultContentType,
                out resolvedContentType,
                out resolvedContentTypeEncoding);

            response.ContentType = resolvedContentType;

            if (result.StatusCode != null)
            {
                response.StatusCode = result.StatusCode.Value;
            }

            _logger.ContentResultExecuting(resolvedContentType);

            if (result.Content != null)
            {
                response.ContentLength = resolvedContentTypeEncoding.GetByteCount(result.Content);

                using (var textWriter = _httpResponseStreamWriterFactory.CreateWriter(response.Body, resolvedContentTypeEncoding))
                {
                    await textWriter.WriteAsync(result.Content);
                    await textWriter.FlushAsync();
                }
            }
        }
    }
}
