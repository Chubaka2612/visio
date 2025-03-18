using Newtonsoft.Json;

namespace Visio.Domain.Core
{
    public abstract class Entity<TKey> : IEntity<TKey>
    {
        [JsonProperty("id")]
        public TKey Id { get; set; }

        [JsonProperty("time_added")]
        public DateTime TimeAdded { get; set; }

        protected Entity()
        {
            TimeAdded = DateTime.UtcNow;
        }
    }
}
