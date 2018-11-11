﻿using Extensions;
using Unianio;
using UnityEngine;
using UnityFunctions;
using Utils;

namespace Main
{
    public class ThreeJoinsInverseKinematics : BaseMainScript
    {
        private Transform _origin,_target,_join1,_join2,_join3;

        const double len1 = 0.30;	    
        const double len2 = 0.20;

        const double capRad = 0.01;

        void Start ()
        {
            
            _origin = 
                fun.meshes.CreatePointyCone(new DtCone {height = 0.05,bottomRadius = 0.05,topRadius = 0.001f})
                    .SetStandardShaderTransparentColor(1,0,1,0.5).transform;
            _origin.position += Vector3.forward*-0.5f;

            _join1 = CreateJoin(1,len1,1,0,0);
            _join2 = CreateJoin(2,len2,0,1,0);
            _join3 = CreateJoin(3,len1,0,0,1);

            _join1.position = _origin.position;
            _join2.position = _join1.position + _origin.forward * (float)len1;
            _join3.position = _join2.position + _origin.forward * (float)len2;

            _target = fun.meshes.CreateBox(new DtBox {side = 0.05}).transform;
            _target.position += Vector3.forward*0.5f;
        }

        void Update ()
        {
            var tarPo = _target.position;
            var oriPo = _origin.position;
            var oriUp = _origin.up;
            var oriFw = _origin.forward;

            // test code STARTS here -----------------------------------------------
            var distToPlane = 
                fun.point.IsBelowPlane(in tarPo, in oriFw, in oriPo) 
                ? 0 
                : fun.point.DistanceToPlane(in tarPo, in oriFw, in oriPo);

            var lenAll = (float)(len1+len2+len1);
            var relDistToPlane = distToPlane / lenAll;
            Vector3 oriRt;
            fun.vector.GetNormal(in oriFw, in oriUp, out oriRt);
            Vector3 tarPoOnPlane;
            fun.point.ProjectOnPlane(in tarPo, in oriRt, in oriPo, out tarPoOnPlane);
            tarPo = Vector3.Lerp(tarPoOnPlane, tarPo, (float)BezierFunc.GetY(relDistToPlane, 0.20,0.00,0.00,1.00));


            Vector3 j0, j1;
            fun.invserseKinematics.ThreeJoinOnVertPlane(oriPo, oriFw, oriUp, tarPo, len1, len2, out j0, out j1);
            // test code ENDS here -------------------------------------------------

            var toJ0 = (j0 - oriPo).normalized;
            var toJ1 = (j1 - j0).normalized;
            var toTarg = (tarPo - j1).normalized;

            _join1.rotation = Quaternion.LookRotation(toJ0, toJ0.GetRealUp(oriUp, oriFw));
Debug.DrawLine(_join1.position+Vector3.one*0.02f,_join1.position+Vector3.one*0.02f+oriFw*1f, Color.blue);
Debug.DrawLine(_join1.position+Vector3.one*0.02f,_join1.position+Vector3.one*0.02f+oriUp*1f, Color.green);

Debug.DrawLine(_join1.position,_join1.position+_join1.forward*0.1f, Color.blue);
Debug.DrawLine(_join1.position,_join1.position+_join1.up*0.1f, Color.green);
            _join2.rotation = Quaternion.LookRotation(toJ1, toJ1.GetRealUp(_join1.rotation*Vector3.up,_join1.rotation*Vector3.forward));
            _join2.position = j0;

            _join3.rotation = Quaternion.LookRotation(toTarg, toTarg.GetRealUp(_join2.rotation*Vector3.up,_join2.rotation*Vector3.forward));
            _join3.position = j1;
        }

        private Transform CreateJoin(int id, double len, double red, double green, double blue)
        {
            var join = 
                fun.meshes.CreateCapsule(new DtCapsule {height = len-capRad*2, name="join_"+id, radius = capRad})
                .SetStandardShaderTransparentColor(red,green,blue,0.5).transform;
            join.gameObject.hideFlags = HideFlags.HideInHierarchy;
            var wrapper = new GameObject(join.name+"_wrapper");
            join.SetParent(wrapper.transform);
            join.transform.localRotation = Quaternion.Euler(90,0,0);
            join.transform.localPosition = new Vector3(0,0,(float)len/2f);
            return wrapper.transform;
        }

        
    }
}