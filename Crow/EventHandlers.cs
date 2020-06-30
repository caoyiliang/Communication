using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Crow
{
    public delegate Task RequestedDataEventHandler<T>(T data);

    public delegate Task RespondedDataEventHandler<T>(T data);
}
