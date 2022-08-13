using System.Collections.Generic;

namespace Topica.RabbitMq.Models
{
    public class ExchangeBinding
    {
        public Exchange Exchange { get; set; }
        public IEnumerable<Binding> Bindings { get; set; }
    }
}