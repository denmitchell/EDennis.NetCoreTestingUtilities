using System;
using System.Collections.Generic;
using System.Text;

namespace EDennis.AspNetCore.Base.EntityFramework {
    public class PagedResult<TEntity> {
        public List<TEntity> Data { get; set; }
        public PagingData PagingData { get; set; }
    }
}
