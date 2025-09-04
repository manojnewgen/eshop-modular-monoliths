using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.DDD
{
    public interface IEntity<T>: IEntity
    {
       public T Id { get; }
    }
    public interface IEntity
    {
        public DateTime? CreatedAt { get; set; }
        public string CreatedBy { get; set; }

        public DateTime? LastModifiedAt { get; set; }

        public string LastModifiedBy { get; set; }


    }
}
