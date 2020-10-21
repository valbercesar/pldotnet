using System;
using System.IO;
using System.Linq;

using System.Collections.Generic;
using System.Threading.Tasks;
using Google.Protobuf;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Caching.Memory;

namespace PlDotNET
{
    public class StorageService : Storage.StorageBase
    {
        private IMemoryCache _cache;
        
        private readonly ILogger<StorageService> _logger;
        public StorageService(IMemoryCache cache, ILogger<StorageService> logger)
        {
            _cache = cache;
            _logger = logger;
        }

        private bool Insert(StorageRequest request)
        {
            // TODO validate the request content here

            if (!_cache.TryGetValue(
                    request.Procedure.FunctionId, 
                    out StorageRequest cachedRequest
                    ) || cachedRequest.Procedure.Source != request.Procedure.Source
            )
            {
                // Set cache options.
                var cacheEntryOptions = new MemoryCacheEntryOptions()
                // Keep in cache for this time, reset time if accessed.
                .SetSlidingExpiration(TimeSpan.FromHours(1))
                .SetSize(128);

                // Save data in cache.
                _cache.Set(request.Procedure.FunctionId, request, cacheEntryOptions);
            }

            return true;
           
        }

        public override Task<StorageResult> Save(StorageRequest request, ServerCallContext context)
        {
            return Task.FromResult(new StorageResult
            {
                Result = Insert(request)
            });
        }

        public override Task<StreamContent> Retrieve(ProcedureInfo info, ServerCallContext context)
        {
            if (_cache.TryGetValue(
                    info.FunctionId, 
                    out StorageRequest cachedRequest
                    ) && cachedRequest.Procedure.Source == info.Source
            )
            {
                return Task.FromResult(
                    cachedRequest.Content
                );
            }

            return Task.FromResult(new StreamContent { Asm = ByteString.CopyFromUtf8("") });
        }
    }
}
