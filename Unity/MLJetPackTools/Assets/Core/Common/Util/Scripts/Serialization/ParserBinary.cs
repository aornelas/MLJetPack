using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using System.Xml;

#if !UNITY_WSA

using System.Runtime.Serialization.Formatters.Binary;
using System.Text; 

namespace MLJetPack.Util {
	public class ParserBinary : ParserBase
	{
		public override string SerializeToString(object objectToSerialize) {
			string binaryString = null; 
			MemoryStream memoryStream = new MemoryStream(); 
			BinaryFormatter bf = new BinaryFormatter ();
			bf.Serialize(memoryStream, objectToSerialize); 
			binaryString = new UTF8Encoding().GetString(memoryStream.ToArray()); 
			return binaryString; 
		}

		public override T DeserializeFromString<T>(string dataString) {
			MemoryStream memoryStream = new MemoryStream(new UTF8Encoding().GetBytes(dataString)); 
			BinaryFormatter serializer = new BinaryFormatter ();
			try {
				return (T)serializer.Deserialize (memoryStream);

			} catch (Exception e) {
				Debug.LogError ("Game data invalid from string: " + dataString + "; " + e.Message);
			}
			return default(T);
		}
	}
}
#endif