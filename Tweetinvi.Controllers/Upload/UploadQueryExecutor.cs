﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Tweetinvi.Core.Injectinvi;
using Tweetinvi.Core.Public.Events;
using Tweetinvi.Core.Public.Models.Enum;
using Tweetinvi.Core.Public.Parameters;
using Tweetinvi.Core.Public.Parameters.Enum;
using Tweetinvi.Core.Web;
using Tweetinvi.Logic.QueryParameters;
using Tweetinvi.Models;
using Tweetinvi.Parameters;

namespace Tweetinvi.Controllers.Upload
{
    public interface IUploadQueryExecutor
    {
        /// <summary>
        /// Upload a binary
        /// </summary>
        Task<IMedia> UploadBinary(byte[] binary, IUploadOptionalParameters parameters = null);

        /// <summary>
        /// Upload a binary
        /// </summary>
        Task<IMedia> UploadBinary(IUploadParameters uploadQueryParameters);

        /// <summary>
        /// Add metadata to a media that has been uploaded.
        /// </summary>
        Task<bool> AddMediaMetadata(IMediaMetadata metadata);

    }

    public class UploadQueryExecutor : IUploadQueryExecutor
    {
        private readonly ITwitterAccessor _twitterAccessor;
        private readonly IFactory<IMedia> _mediaFactory;
        private readonly IFactory<IChunkedUploader> _chunkedUploadFactory;
        private readonly IUploadHelper _uploadHelper;

        public UploadQueryExecutor(
            ITwitterAccessor twitterAccessor,
            IFactory<IMedia> mediaFactory,
            IFactory<IChunkedUploader> chunkedUploadFactory,
            IUploadHelper uploadHelper)
        {
            _twitterAccessor = twitterAccessor;
            _mediaFactory = mediaFactory;
            _chunkedUploadFactory = chunkedUploadFactory;
            _uploadHelper = uploadHelper;
        }


        public Task<IMedia> UploadBinary(byte[] binary, IUploadOptionalParameters parameters)
        {
            var uploadParameters = CreateUploadParametersFromOptionalParameters(binary, parameters);

            return UploadBinary(uploadParameters);
        }

        public async Task<IMedia> UploadBinary(IUploadParameters uploadQueryParameters)
        {
            var binary = uploadQueryParameters.Binary;
            var uploader = CreateChunkedUploader();

            var initParameters = new ChunkUploadInitParameters
            {
                TotalBinaryLength = binary.Length,
                MediaType = uploadQueryParameters.QueryMediaType,
                MediaCategory = uploadQueryParameters.QueryMediaCategory,
                AdditionalOwnerIds = uploadQueryParameters.AdditionalOwnerIds,
                CustomRequestParameters = uploadQueryParameters.InitCustomRequestParameters,
            };

            var initOperationSucceeded = await uploader.Init(initParameters);

            if (initOperationSucceeded)
            {
                var binaryChunks = GetBinaryChunks(binary, uploadQueryParameters.MaxChunkSize);

                var totalSize = binary.Length;
                var uploadedSize = 0;

                uploadQueryParameters.UploadStateChanged?.Invoke(new UploadStateChangedEventArgs(UploadProgressState.INITIALIZED, 0, totalSize));

                for (int i = 0; i < binaryChunks.Count; ++i)
                {
                    var binaryChunk = binaryChunks[i];

                    var appendParameters = new ChunkUploadAppendParameters(
                        binaryChunk,
                        "media", // Must be `media`, if using the real media type as content id, Twitter does not accept when invoking .Finalize().
                        uploadQueryParameters.Timeout)
                    {
                        UploadProgressChanged = (current, total) =>
                        {
                            uploadQueryParameters.UploadStateChanged?.Invoke(new UploadStateChangedEventArgs(UploadProgressState.PROGRESS_CHANGED, uploadedSize + current, totalSize));
                        }
                    };

                    appendParameters.CustomRequestParameters = uploadQueryParameters.AppendCustomRequestParameters;

                    var appendOperationSucceeded = await uploader.Append(appendParameters);

                    if (!appendOperationSucceeded)
                    {
                        uploadQueryParameters.UploadStateChanged?.Invoke(new UploadStateChangedEventArgs(UploadProgressState.FAILED, uploadedSize, totalSize));

                        return null;
                    }

                    uploadedSize += binaryChunk.Length;
                }

                var media = await uploader.Complete();

                uploadQueryParameters.UploadStateChanged?.Invoke(new UploadStateChangedEventArgs(UploadProgressState.COMPLETED, uploadedSize, totalSize));

                var category = uploadQueryParameters.MediaCategory;
                var isAwaitableUpload = category == MediaCategory.Gif || category == MediaCategory.Video;
                if (isAwaitableUpload && uploadQueryParameters.WaitForTwitterProcessing)
                {
                    await _uploadHelper.WaitForMediaProcessingToGetAllMetadata(media);
                }

                return media;
            }

            return _mediaFactory.Create();
        }

        private List<byte[]> GetBinaryChunks(byte[] binary, int chunkSize)
        {
            var result = new List<byte[]>();
            var numberOfChunks = (int)Math.Ceiling((double)binary.Length / chunkSize);

            for (int i = 0; i < numberOfChunks; ++i)
            {
                var skip = i * chunkSize;
                var take = Math.Min(chunkSize, binary.Length - skip);

                var elts = binary.Skip(skip).Take(take).ToArray();

                result.Add(elts);
            }

            return result;
        }

        /// <summary>
        /// Create a chunked uploader that give developers access to Twitter chunked uploads
        /// </summary>
        private IChunkedUploader CreateChunkedUploader()
        {
            return _chunkedUploadFactory.Create();
        }

        public Task<bool> AddMediaMetadata(IMediaMetadata metadata)
        {
            var json = JsonConvert.SerializeObject(metadata);
            return _twitterAccessor.TryPOSTJsonContent("https://upload.twitter.com/1.1/media/metadata/create.json", json);
        }

        private static UploadParameters CreateUploadParametersFromOptionalParameters(
            byte[] binary,
            IUploadOptionalParameters parameters)
        {
            var uploadParameters = new UploadParameters(binary);

            if (parameters != null)
            {
                uploadParameters.AdditionalOwnerIds = parameters.AdditionalOwnerIds;
                uploadParameters.AppendCustomRequestParameters = parameters.AppendCustomRequestParameters;
                uploadParameters.InitCustomRequestParameters = parameters.InitCustomRequestParameters;
                uploadParameters.MaxChunkSize = parameters.MaxChunkSize;
                uploadParameters.QueryMediaCategory = parameters.QueryMediaCategory;
                uploadParameters.QueryMediaType = parameters.QueryMediaType;
                uploadParameters.Timeout = parameters.Timeout;
                uploadParameters.UploadStateChanged = parameters.UploadStateChanged;
                uploadParameters.WaitForTwitterProcessing = parameters.WaitForTwitterProcessing;
            }

            return uploadParameters;
        }
    }
}