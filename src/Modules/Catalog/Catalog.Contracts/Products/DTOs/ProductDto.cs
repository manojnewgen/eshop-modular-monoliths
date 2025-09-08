using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Catalog.Contracts.Products.DTOs
{
    public record ProductDto(
         Guid Id,
         string Name,
         string Description,
         decimal Price,
         string ImageFile,
         List<string> Categories,
         int StockQuantity
     );
}
