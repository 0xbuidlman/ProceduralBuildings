﻿using UnityEngine;

namespace Thesis {

public sealed class NeoclassicalBalconyDoor : Base.BalconyDoor
{
  /*************** CONSTRUCTORS ***************/

  public NeoclassicalBalconyDoor (Base.Face parent, Vector3 dr, Vector3 dl, ComponentCoordinate position)
    : base(parent, position)
  {
    height = ((Neoclassical) parentBuilding).balconyHeight;

    boundaries.Add(dr);
    boundaries.Add(dl);
    boundaries.Add(new Vector3(dl.x, dl.y + height, dl.z));
    boundaries.Add(new Vector3(dr.x, dr.y + height, dr.z));

    frame = new Base.BalconyFrame(this);
    body = new Base.Glass(this);

    balconyFloor = new Base.BalconyFloor(this);
  }

  /*************** METHODS ***************/

  public override void Draw ()
  {
    base.Draw();

    body.FindVertices();
    body.FindTriangles();
    body.Draw();

    frame.FindVertices();
    frame.FindTriangles();
    frame.Draw();

    balconyFloor.FindVertices();
    balconyFloor.FindTriangles();
    balconyFloor.Draw();
  }
}

} // namespace Thesis