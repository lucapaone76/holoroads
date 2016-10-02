using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using AssemblyCSharp;

namespace AssemblyCSharpEditor
{
	//inspired by http://www.sitepoint.com/saving-and-loading-player-game-data-in-unity/
	// http://gamedevelopment.tutsplus.com/tutorials/how-to-save-and-load-your-players-progress-in-unity--cms-20934
	public class SaveTde
	{
		static List<TDEStrt> listTDEStrt = new List<TDEStrt> ();

		static public void SetListTDEStrt(List<TDEStrt> _listTDEStrt){
			listTDEStrt = _listTDEStrt;
		}
		static public List<TDEStrt> GetListTDEStrt(){
			return listTDEStrt;
		}

		static public void SaveListTDEStrt (List<TDEStrt> list)
		{
			BinaryFormatter bf = new BinaryFormatter();
			//Application.persistentDataPath is a string, so if you wanted you can put that into debug.log if you want to know where save games are located
			Debug.Log("saving in "+Application.persistentDataPath + "/savedGames.gd");
			FileStream file = File.Create (Application.persistentDataPath + "/savedGames.gd"); //you can call it anything you want
			bf.Serialize(file, list);
			file.Close();
		}

		public static List<TDEStrt> LoadTde() {
			Debug.Log("trying to load from  "+Application.persistentDataPath + "/savedGames.gd");
			if(File.Exists(Application.persistentDataPath + "/savedGames.gd")) {
				BinaryFormatter bf = new BinaryFormatter();
				FileStream file = File.Open(Application.persistentDataPath + "/savedGames.gd", FileMode.Open);
				SaveTde.listTDEStrt = (List<TDEStrt>)bf.Deserialize(file);
				file.Close();
			}
			return SaveTde.listTDEStrt;

		}
	}
}

