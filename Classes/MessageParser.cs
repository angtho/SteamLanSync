using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace SteamLanSync.Messages
{
    public class MessageParser
    {
        /// <summary>
        /// Parses a message contained within a string buffer, removes the message data from the buffer, and returns the message object.
        /// </summary>
        /// <param name="buf">The buffer from which a message will be extracted</param>
        /// <returns>The Message object parsed from the buffer, or null if no message was contained in the buffer</returns>
        public static Message Parse(ref string buf)
        {
            if (buf.Length == 0)
                return null;

            // search for the start-of-message delimiter
            string delim = Message.MESSAGE_START_DELIMITER;
            int posFirstDelim = buf.IndexOf(delim);
            if (posFirstDelim < 0) // could not find start-of-message delimiter
                return null;

            buf = buf.Substring(posFirstDelim); // discard any data before the delimiter (may have been a previous corrupted message)
            
            // see if we can find another start-of-message delimiter 
            // (if so, the buffer contains more than one message, or it contains one full message and part of another one)
            int posSecondDelim = buf.IndexOf(delim, delim.Length); // we expect the message to start with a delimiter, so skip that one
            string messageStr;
            if (posSecondDelim < 0) // no second delimiter, use the whole buffer
                messageStr = buf.Substring(delim.Length);
            else
                messageStr = buf.Substring(delim.Length, posSecondDelim - delim.Length); // second delimiter found, only take data up to there

            int posBreak = messageStr.IndexOf('\n');
            if (posBreak < 0)
                return null;

            string messageType = messageStr.Substring(0, posBreak);
            string messageContent = messageStr.Substring(posBreak);

            if (messageType.Length == 0)
                return null;

            Message parsedMsg = null;

            try
            {
                switch (messageType)
                {
                    case "HELLO":
                        parsedMsg = JsonConvert.DeserializeObject<HelloMessage>(messageContent);
                        break;
                    case "REQUEST_APP_LIST":
                        parsedMsg = JsonConvert.DeserializeObject<RequestAppListMessage>(messageContent);
                        break;
                    case "APP_LIST":
                        parsedMsg = JsonConvert.DeserializeObject<AppListMessage>(messageContent);
                        break;
                    case "REQUEST_APP_TRANSFER":
                        parsedMsg = JsonConvert.DeserializeObject<RequestAppTransferMessage>(messageContent);
                        break;
                    case "START_APP_TRANSFER":
                        parsedMsg = JsonConvert.DeserializeObject<StartAppTransferMessage>(messageContent);
                        break;
                    case "CANCEL_APP_TRANSFER":
                        parsedMsg = JsonConvert.DeserializeObject<CancelAppTransferMessage>(messageContent);
                        break;
                    case "GOODBYE":
                        parsedMsg = JsonConvert.DeserializeObject<GoodbyeMessage>(messageContent);
                        break;
                    default:
                        parsedMsg = null;
                        break;
                }
            }
            catch (JsonReaderException) // we expect this if the message on the buffer is incomplete (e.g. "HELLO\n{'hostname':'someth"
            {
                parsedMsg = null;
            }
            catch (JsonSerializationException) // we expect this if the message on the buffer is incomplete (e.g. "HELLO\n{'hostname':'someth"
            {
                parsedMsg = null;
            }
            catch (JsonException) // we expect this if the message on the buffer is incomplete (e.g. "HELLO\n{'hostname':'someth"
            {
                parsedMsg = null;
            }
            

            if (parsedMsg != null) // we parsed a message, so remove that data from the buffer
                buf = buf.Substring(delim.Length + messageStr.Length);

            return parsedMsg;
        }

    }


}
