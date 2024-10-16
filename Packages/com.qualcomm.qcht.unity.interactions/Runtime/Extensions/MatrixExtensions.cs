// /******************************************************************************
//  * File: MatrixExtensions.cs
//  * Copyright (c) 2023 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
//  *
//  * Confidential and Proprietary - Qualcomm Technologies, Inc.
//  *
//  ******************************************************************************/

using UnityEngine;

namespace QCHT.Interactions.Extensions
{
    public static class MatrixExtensions
    {
        public static Matrix4x4 ConvertSVRToUnity(this Matrix4x4 matrix)
        {
            Matrix4x4 inverted = Matrix4x4.identity;
            inverted[0, 0] = matrix[0, 0];
            inverted[0, 1] = matrix[0, 1];
            inverted[0, 2] = -matrix[0, 2];
            inverted[0, 3] = matrix[0, 3];
            
            inverted[1, 0] = matrix[1, 0];
            inverted[1, 1] = matrix[1, 1];
            inverted[1, 2] = -matrix[1, 2];
            inverted[1, 3] = matrix[1, 3];
            
            inverted[2, 0] = -matrix[2, 0];
            inverted[2, 1] = -matrix[2, 1];
            inverted[2, 2] = matrix[2, 2];
            inverted[2, 3] = -matrix[2, 3];
            
            inverted[3, 0] = matrix[3, 0];
            inverted[3, 1] = matrix[3, 1];
            inverted[3, 2] = -matrix[3, 2];
            inverted[3, 3] = matrix[3, 3];

            return inverted;
        }
        
        public static void ApplyToTransform(this Matrix4x4 matrix, Transform transform, bool worldSpace = true)
        {
            transform.localScale = matrix.GetScale();
            if (worldSpace)
            {
                transform.rotation = matrix.GetRotation();
                transform.position = matrix.GetPosition();    
            }
            else
            {
                transform.localRotation = matrix.GetRotation();
                transform.localPosition = matrix.GetPosition();
            }
        }
        
        public static Quaternion GetRotation(this Matrix4x4 matrix)
        {
            Vector3 f;
            f.x = matrix.m02;
            f.y = matrix.m12;
            f.z = matrix.m22;

            Vector3 up;
            up.x = matrix.m01;
            up.y = matrix.m11;
            up.z = matrix.m21;

            return Quaternion.LookRotation(f, up);
        }

        public static Vector3 GetPosition(this Matrix4x4 matrix)
        {
            Vector3 p;
            p.x = matrix.m03;
            p.y = matrix.m13;
            p.z = matrix.m23;
            return p;
        }

        public static Vector3 GetScale(this Matrix4x4 matrix)
        {
            Vector3 s;
            s.x = new Vector4(matrix.m00, matrix.m10, matrix.m20, matrix.m30).magnitude;
            s.y = new Vector4(matrix.m01, matrix.m11, matrix.m21, matrix.m31).magnitude;
            s.z = new Vector4(matrix.m02, matrix.m12, matrix.m22, matrix.m32).magnitude;
            return s;
        }
        
        public static void LogMatrix(this Matrix4x4 m, string name)
        {
            var mLogs =
                $"[0,0]:{m[0, 0]} - [0,1]:{m[0, 1]} - [0,2]:{m[0, 2]} - [0,3]:{m[0, 3]} - [1,0]:{m[1, 0]} - [1,1]:{m[1, 1]} - [1,2]:{m[1, 2]} - [1,3]:{m[1, 3]} - [2,0]:{m[2, 0]} - [2,1]:{m[2, 1]} - [2,2]:{m[2, 2]} - [2,3]:{m[2, 3]} - [3,0]:{m[3, 0]} - [3,1]:{m[3, 1]} - [3,2]:{m[3, 2]} - [3,3]:{m[3, 3]}";
            Debug.LogError(mLogs);
        }
    }
}