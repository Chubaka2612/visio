using System.Net;
using Microsoft.Azure.Cosmos;

namespace Visio.Data.Core.Db
{
    public static class ItemResponseResultExtensions
    {
        public static void EnsureSuccessStatusCode<TEntity>(this ItemResponse<TEntity> itemResponse)
        {
            if (itemResponse.StatusCode is not (HttpStatusCode.Created or HttpStatusCode.OK or HttpStatusCode.NoContent))
            {
                throw new HttpRequestException(
                    $"Cosmos operation failed with status code: {itemResponse.StatusCode}");
            }
        }
    }
}