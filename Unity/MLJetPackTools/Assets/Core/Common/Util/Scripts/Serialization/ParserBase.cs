using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;

#if !UNITY_EDITOR && UNITY_METRO
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;
#endif

public class ParserBase {
	public string SaveToFilePath(byte[] bytes, string persistentPath) {
		using (Stream stream = OpenFileForWrite(persistentPath))
		{
			stream.Write (bytes, 0, bytes.Length);
			stream.Flush();
		}
		return persistentPath;
	}

	public string SaveToPersistentDataStorage(byte[] bytes, string filePath) {
		string persistentPath = Path.Combine (Application.persistentDataPath, filePath);
		using (Stream stream = OpenFileForWrite(persistentPath))
		{
			stream.Write (bytes, 0, bytes.Length);
			stream.Flush();
		}
		return persistentPath;
	}

	#region Serialization
	public bool SerializeToPersistentDataStorage(object objectToSerialize, string filePath) {
		string persistentPath = Path.Combine (Application.persistentDataPath, filePath);
		if (persistentPath != null) {
			return SerializeToFilePath (objectToSerialize, persistentPath);
		}
		return false;
	}

	public bool SerializeToFilePath(object objectToSerialize, string filePath)
	{
		using (Stream stream = OpenFileForWrite(filePath))
		{
			string jsonString = SerializeToString (objectToSerialize);
			var bytes = System.Text.Encoding.UTF8.GetBytes(jsonString);
			stream.Write (bytes, 0, bytes.Length);
			stream.Flush();
		}
		return true;
	}

	public T DeserializeFromPersistentDataStorage<T>(string filePath) {
		string persistentPath = Path.Combine (Application.persistentDataPath, filePath);
		if (persistentPath != null) {
			return DeserializeFromFilePath<T>(persistentPath);
		}
		return default(T);
	}

	public T DeserializeFromFilePath<T>(string filePath) {
		if (File.Exists (filePath)) {
			try {
				byte[] bytes = File.ReadAllBytes (filePath);
				if (bytes.GetLength (0) > 0) {
					string dataString = System.Text.Encoding.UTF8.GetString(bytes);
					//							Debug.Log("Data String: " + dataString);
					return DeserializeFromString<T>(dataString);
				}

			} catch (Exception e) {
				Debug.LogError ("Deserialization error to: " + filePath + "; " + e.Message);
			}
		}
		return default(T);
	}
	#endregion

	/// <summary>
	/// Opens the specified file for reading.
	/// </summary>
	/// <param name="folderName">The name of the folder containing the file.</param>
	/// <param name="fileName">The name of the file, including extension. </param>
	/// <returns>Stream used for reading the file's data.</returns>
	private Stream OpenFileForRead(string filePath)
	{
		Stream stream = null;
		string folderName = Path.GetDirectoryName(filePath);
		string fileName = Path.GetFileName(filePath);
#if !UNITY_EDITOR && UNITY_METRO
		Task<Task> task = Task<Task>.Factory.StartNew(
		async () =>
		{
		    StorageFolder folder = await StorageFolder.GetFolderFromPathAsync(folderName);
		    StorageFile file = await folder.GetFileAsync(fileName);
            IRandomAccessStreamWithContentType randomAccessStream = await file.OpenReadAsync();
            stream = randomAccessStream.AsStreamForRead();
        });
		task.Wait();
		task.Result.Wait();
#else
        stream = new FileStream(Path.Combine(folderName, fileName), FileMode.Open, FileAccess.Read);
		#endif
		return stream;
	}

	/// <summary>
	/// Opens the specified file for writing.
	/// </summary>
	/// <param name="folderName">The name of the folder containing the file.</param>
	/// <param name="fileName">The name of the file, including extension.</param>
	/// <returns>Stream used for writing the file's data.</returns>
	/// <remarks>If the specified file already exists, it will be overwritten.</remarks>
	private Stream OpenFileForWrite(string filePath)
	{
		Stream stream = null;

		string folderName = Path.GetDirectoryName(filePath);
		string fileName = Path.GetFileName(filePath);

#if !UNITY_EDITOR && UNITY_METRO
		Task<Task> task = Task<Task>.Factory.StartNew(
		async () =>
		{
            StorageFolder folder = await StorageFolder.GetFolderFromPathAsync(folderName);
            StorageFile file = await folder.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting);
            IRandomAccessStream randomAccessStream = await file.OpenAsync(FileAccessMode.ReadWrite);
            stream = randomAccessStream.AsStreamForWrite();
        });
		task.Wait();
		task.Result.Wait();
#else
        stream = new FileStream(Path.Combine(folderName, fileName), FileMode.Create, FileAccess.Write);
		#endif
		return stream;
	}

	public virtual string SerializeToString(object objectToSerialize) {
		return null;
	}

	public virtual T DeserializeFromString<T>(string dataString) {
		return default(T);
	}
}
