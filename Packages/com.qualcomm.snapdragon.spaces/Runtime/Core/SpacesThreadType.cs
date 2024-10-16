/******************************************************************************
 * File: SpacesThreadType.cs
 * Copyright (c) 2023 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
 *
 * Confidential and Proprietary - Qualcomm Technologies, Inc.
 *
 ******************************************************************************/

namespace Qualcomm.Snapdragon.Spaces
{
    /// <summary>
    ///     SpacesThreadType can be used to identify a running thread
    ///     in order to adjust its scheduling priority with OpenXR
    /// </summary>
    public enum SpacesThreadType : ushort
    {
        // normal thread priority
        SPACES_THREAD_TYPE_APPLICATION_MAIN = 1,
        SPACES_THREAD_TYPE_APPLICATION_WORKER = 2,
        SPACES_THREAD_TYPE_RENDERER_MAIN = 3,

        // higher thread priority
        SPACES_THREAD_TYPE_RENDERER_WORKER = 4
    }
}
