using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using System.Text; 

namespace MLJetPack.Util {
	public class ParserXML : ParserBase
	{
		public override string SerializeToString(object objectToSerialize) {
			string xmlString = null; 
			MemoryStream memoryStream = new MemoryStream(); 
			XmlSerializer xs = new XmlSerializer(objectToSerialize.GetType()); 
			XmlTextWriter xmlTextWriter = new XmlTextWriter(memoryStream, Encoding.UTF8); 
			xs.Serialize(xmlTextWriter, objectToSerialize); 
			memoryStream = (MemoryStream)xmlTextWriter.BaseStream; 
			xmlString = new UTF8Encoding().GetString(memoryStream.ToArray()); 
			return xmlString; 
		}
			
		public override T DeserializeFromString<T>(string dataString) {
			XmlSerializer serializer = new XmlSerializer (typeof(T));
			try {
				TextReader textReader = new StringReader (dataString);
				return (T)serializer.Deserialize (textReader);

			} catch (Exception e) {
				Debug.LogError ("Game data invalid from string: " + dataString + "; " + e.Message);
			}
			return default(T);
		}
	}
}