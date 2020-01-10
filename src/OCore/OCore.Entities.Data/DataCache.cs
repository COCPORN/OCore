using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace OCore.Entities.Data
{
    public class DataCache<T>
    {
        public DataCache(IDataEntity<T> dataSource)
        {
            this.dataSource = dataSource;
        }

        public Type DataSourceType { get; set; } = typeof(T);

        public DateTimeOffset RefreshedAt { get; set; }

        public TimeSpan CacheFor { get; set; }

        IDataEntity<T> dataSource;

        public async Task Refresh()
        {
            if (DateTimeOffset.UtcNow - RefreshedAt > CacheFor)
            {
                data = await dataSource.Read();
            }
            RefreshedAt = DateTimeOffset.UtcNow;
        }

        T data;
        public T Data
        {
            get
            {
                if (DateTimeOffset.UtcNow - RefreshedAt > CacheFor)
                {
                    dataSource.Read().ContinueWith(x =>
                    {
                        data = x.Result;
                        RefreshedAt = DateTimeOffset.UtcNow;
                    }, TaskContinuationOptions.OnlyOnRanToCompletion);
                }
                return data;
            }
            set
            {
                data = value;
            }
        }
    }
}
