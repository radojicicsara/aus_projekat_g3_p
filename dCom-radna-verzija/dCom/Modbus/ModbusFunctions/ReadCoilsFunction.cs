using Common;
using Modbus.FunctionParameters;
using System;
using System.Collections.Generic;
using System.Net;
using System.Reflection;

namespace Modbus.ModbusFunctions
{
    /// <summary>
    /// Class containing logic for parsing and packing modbus read coil functions/requests.
    /// </summary>
    public class ReadCoilsFunction : ModbusFunction
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ReadCoilsFunction"/> class.
        /// </summary>
        /// <param name="commandParameters">The modbus command parameters.</param>
		public ReadCoilsFunction(ModbusCommandParameters commandParameters) : base(commandParameters)
        {
            CheckArguments(MethodBase.GetCurrentMethod(), typeof(ModbusReadCommandParameters));
        }

        /// <inheritdoc/>
        public override byte[] PackRequest()
        {
            //TO DO: IMPLEMENT
            byte[] ret_val = new byte[12];
            Buffer.BlockCopy(BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)CommandParameters.TransactionId)), 0, ret_val, 0, 2);
            Buffer.BlockCopy(BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)CommandParameters.ProtocolId)), 0, ret_val, 2, 2);
            Buffer.BlockCopy(BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)CommandParameters.Length)), 0, ret_val, 4, 2);

            ret_val[6] = CommandParameters.UnitId;
            ret_val[7] = CommandParameters.FunctionCode;
            Buffer.BlockCopy(
                BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)((ModbusReadCommandParameters)CommandParameters).StartAddress)),
                0, ret_val, 8, 2);

            Buffer.BlockCopy(
                BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)((ModbusReadCommandParameters)CommandParameters).Quantity)),
                0, ret_val, 10, 2);

            return ret_val;
        }

        /// <inheritdoc />
        public override Dictionary<Tuple<PointType, ushort>, ushort> ParseResponse(byte[] response)
        {
            //TO DO: IMPLEMENT
            Dictionary<Tuple<PointType, ushort>, ushort> r = new Dictionary<Tuple<PointType, ushort>, ushort>();
            if (response[7] != CommandParameters.FunctionCode + 0x80)
            {
                ushort start_address = ((ModbusReadCommandParameters)CommandParameters).StartAddress;
                int count = 0;
                int byte_count = response[8];
                int mask = 1;
                for (int i = 0; i < byte_count; i++)
                {
                    byte temp = response[9 + i];
                    for (int j = 0; j < 8; j++)
                    {
                        ushort value = (ushort)(temp & mask);
                        r.Add(new Tuple<PointType, ushort>(PointType.DIGITAL_OUTPUT, start_address), value);
                        temp >>= 1;
                        count++;
                        start_address++;
                    }
                    ushort quantity = ((ModbusReadCommandParameters)CommandParameters).Quantity;
                    if (quantity <= count)
                    {
                        break;
                    }
                }
            }
            else
            {
                HandeException(response[8]);
            }
            return r;
        }
    }
}
    