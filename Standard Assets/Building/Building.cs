﻿using UnityEngine;
using System.Collections.Generic;

using CombinablesCollection = System.Collections.Generic.IList<Thesis.Interface.ICombinable>;

namespace Thesis {

public class Building
{
  private Material _windowMaterial;
  public Material windowMaterial
  {
    get { return _windowMaterial; }
  }

  private Material _balconyDoorMaterial;
  public Material balconyDoorMaterial
  {
    get { return _balconyDoorMaterial; }
  }

  private Material _doorMaterial;
  public Material doorMaterial
  {
    get { return _doorMaterial; }
  }

  private Material _shutterMaterial;
  public Material shutterMaterial
  {
    get { return _shutterMaterial; }
  }

  private Material _compDecorMaterial;
  public Material compDecorMaterial
  {
    get { return _compDecorMaterial; }
  }

  private Material _simpleCompDecorMaterial;
  public Material simpleCompDecorMaterial
  {
    get { return _simpleCompDecorMaterial; }
  }

  public GameObject gameObject;

  public BuildingMesh buildingMesh;

  private Dictionary<string, CombinablesCollection> _combinables;

  private Dictionary<string, GameObject> _combiners;

  public Building (Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4, bool editor = false)
  {
    Init();

    // must be _after_ the initialization of this object
    if (!editor)
    {
      buildingMesh = new BuildingMesh(this, p1, p2, p3, p4);
      gameObject = new GameObject("Neoclassical");
      gameObject.transform.position = buildingMesh.meshOrigin;
    }
  }

  public Building (BuildingLot lot, bool editor = false)
  {
    Init();

    // must be _after_ the initialization of this object
    if (!editor)
    {
      buildingMesh = new BuildingMesh(this, lot);
      gameObject = new GameObject("Neoclassical");
      gameObject.transform.position = buildingMesh.meshOrigin;
    }
  }

  private void Init()
  {
    _combinables = new Dictionary<string, CombinablesCollection>();
    _combiners = new Dictionary<string, GameObject>();

    // find window material randomly
    var list = MaterialManager.Instance.GetCollection("mat_neo_window");
    var num = Random.Range(0, list.Count);
    _windowMaterial = list[num];

    // balcony door material
    list = MaterialManager.Instance.GetCollection("mat_neo_balcony_door");
    _balconyDoorMaterial = list[num];

    // door material
    list = MaterialManager.Instance.GetCollection("mat_neo_door");
    num = Random.Range(0, list.Count);
    _doorMaterial = list[num];

    // shutter material
    // not randomly selected, depends on the door material
    // count and shutter material count, in order to match
    // colours
    list = MaterialManager.Instance.GetCollection("mat_neo_shutter");
    var doorCount = MaterialManager.Instance.GetCollection("mat_neo_door").Count;
    var shutCount = MaterialManager.Instance.GetCollection("mat_neo_shutter").Count;
    _shutterMaterial = list[num * shutCount / doorCount];

    // component decor material
    list = MaterialManager.Instance.GetCollection("mat_comp_decor");
    num = Random.Range(0, list.Count);
    _compDecorMaterial = list[num];
    //compDecorMaterial = list[list.Count - 1];

    list = MaterialManager.Instance.GetCollection("mat_comp_decor_simple");
    num = Random.Range(0, list.Count);
    _simpleCompDecorMaterial = list[num];
  }

  public void AddCombinable(string materialName, Interface.ICombinable combinable)
  {
    if (!_combinables.ContainsKey(materialName))
    {
      CombinablesCollection temp = new List<Interface.ICombinable>();
      temp.Add(combinable);
      _combinables.Add(materialName, temp);
    }
    else
      _combinables[materialName].Add(combinable);
  }

  public void CombineSubmeshes ()
  {
    foreach (string key in _combinables.Keys)
    {
      if (_combinables[key].Count < 2) continue;

      var gobject = new GameObject(key + "_combiner");
      gobject.active = false;
      gobject.transform.parent = gameObject.transform;
      var filter = gobject.AddComponent<MeshFilter>();
      var renderer = gobject.AddComponent<MeshRenderer>();
      renderer.sharedMaterial = _combinables[key][0].material;

      var filters = new MeshFilter[_combinables[key].Count];
      for (var i = 0; i < _combinables[key].Count; ++i)
      {
        filters[i] = _combinables[key][i].meshFilter;
        GameObject.Destroy(_combinables[key][i].mesh);
        GameObject.Destroy(_combinables[key][i].gameObject);
      }

      var combine = new CombineInstance[filters.Length];
      for (var i = 0; i < filters.Length; ++i)
      {
        combine[i].mesh = filters[i].sharedMesh;
        combine[i].transform = filters[i].transform.localToWorldMatrix;
      }

      //filter.mesh = new Mesh();
      filter.mesh.CombineMeshes(combine);

      if (_combiners.ContainsKey(key))
        _combiners[key] = gobject;
      else
        _combiners.Add(key, gobject);
    }
    _combinables.Clear();
  }

  public void Draw ()
  {
    buildingMesh.FindVertices();
    buildingMesh.FindTriangles();
    buildingMesh.Draw();
    gameObject.SetActiveRecursively(true);
  }

  public void Destroy ()
  {
    GameObject.DestroyImmediate(gameObject);

    foreach (string key in _combinables.Keys)
      for (var i = 0; i < _combinables[key].Count; ++i)
      {
        GameObject.DestroyImmediate(_combinables[key][i].mesh);
        GameObject.DestroyImmediate(_combinables[key][i].gameObject);
      }

    foreach (GameObject go in _combiners.Values)
    {
      if (go != null)
      {
        GameObject.DestroyImmediate(go.GetComponent<MeshFilter>().mesh);
        GameObject.DestroyImmediate(go);
      }
    }

    _combinables.Clear();
    _combiners.Clear();
    buildingMesh.Destroy();
  }
}

} // namespace Thesis