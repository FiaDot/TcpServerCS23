using System;
using System.Collections;
using System.Collections.Generic;
using Google.Protobuf.Protocol;
using UnityEngine;

public class ObjectManager
{
	public NetCharacter MyPlayer { get; set; }
	Dictionary<int, GameObject> _objects = new Dictionary<int, GameObject>();
	// List<GameObject> _objects = new List<GameObject>();

	public void Add(PlayerInfo info, bool myPlayer = false)
	{
		if (myPlayer)
		{
			GameObject go = Managers.Resource.Instantiate("Creature/MyPlayer");
			go.name = info.Name;
			_objects.Add(info.PlayerId, go);

			MyPlayer = go.GetComponent<NetCharacter>();
			MyPlayer.Id = info.PlayerId;
			MyPlayer.IsMine = true;
			// MyPlayer.NetMoveInfo = info.NetMoveInfo;
			MyPlayer.InitPos(info.NetMoveInfo);
		}
		else
		{
			GameObject go = Managers.Resource.Instantiate("Creature/Player");
			go.name = info.Name;
			_objects.Add(info.PlayerId, go);

			NetCharacter pc = go.GetComponent<NetCharacter>();
			pc.Id = info.PlayerId;
			// pc.NetMoveInfo = info.NetMoveInfo;
			pc.IsMine = false;
			pc.InitPos(info.NetMoveInfo);
		}
	}

	public void Add(int id, GameObject go)
	{
		_objects.Add(id, go);
	}

	public void Remove(int id)
	{
		GameObject go = FindById(id);
		if (go == null)
			return;
		
		_objects.Remove(id);
		Managers.Resource.Destroy(go);
	}

	public void RemoveMyPlayer()
	{
		if (MyPlayer == null)
			return;

		Remove(MyPlayer.Id);
		MyPlayer = null;
	}

	// public void Remove(GameObject go)
	// {
	// 	_objects.Remove(go);
	// }

	public GameObject Find(Vector3Int cellPos)
	{
		foreach (GameObject obj in _objects.Values)
		{
			NetCharacter cc = obj.GetComponent<NetCharacter>();
			if (cc == null)
				continue;

			// if (cc.CellPos == cellPos)
			// 	return obj;
		}

		return null;
	}

	public GameObject Find(Func<GameObject, bool> condition)
	{
		foreach (GameObject obj in _objects.Values)
		{
			if (condition.Invoke(obj))
				return obj;
		}

		return null;
	}

	public GameObject FindById(int id)
	{
		GameObject go = null;
		_objects.TryGetValue(id, out go);
		return go;
	}

	public void Clear()
	{
		foreach (GameObject obj in _objects.Values)
			Managers.Resource.Destroy(obj);
		
		_objects.Clear();
	}
}
