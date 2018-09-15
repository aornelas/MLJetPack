using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;

namespace MLJetPack.Util {
	public class ParserJson : ParserBase
	{
		public override string SerializeToString(object objectToSerialize) {
			return JsonUtility.ToJson (objectToSerialize);
		}

		public override T DeserializeFromString<T>(string dataString) {
			try {
				T gameData = JsonUtility.FromJson<T>(dataString);
				return gameData;

			} catch (Exception e) {
				Debug.LogError ("Game data invalid from string: " + dataString + "; " + e.Message);
			}
			return default(T);
		}
	}
}