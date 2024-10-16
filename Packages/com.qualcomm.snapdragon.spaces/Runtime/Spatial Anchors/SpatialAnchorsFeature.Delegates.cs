/******************************************************************************
 * File: SpatialAnchorsFeature.Delegates.cs
 * Copyright (c) 2022 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
 *
 * Confidential and Proprietary - Qualcomm Technologies, Inc.
 *
 ******************************************************************************/

using System;
using System.Runtime.InteropServices;

namespace Qualcomm.Snapdragon.Spaces
{
    internal sealed partial class SpatialAnchorsFeature
    {
        #region XR_MSFT_spatial_anchor bindings

        [UnmanagedFunctionPointerAttribute(CallingConvention.Cdecl)]
        private delegate XrResult CreateSpatialAnchorMSFTDelegate(ulong session, XrSpatialAnchorCreateInfoMSFT createInfoMSFT, ref ulong anchor);

        private static CreateSpatialAnchorMSFTDelegate _xrCreateSpatialAnchorMSFTPtr;

        [UnmanagedFunctionPointerAttribute(CallingConvention.Cdecl)]
        private delegate XrResult CreateSpatialAnchorSpaceMSFTDelegate(ulong session, XrSpatialAnchorSpaceCreateInfoMSFT createInfo, ref ulong space);

        private static CreateSpatialAnchorSpaceMSFTDelegate _xrCreateSpatialAnchorSpaceMSFTPtr;

        private delegate XrResult DestroySpatialAnchorMSFTDelegate(ulong anchor);

        private static DestroySpatialAnchorMSFTDelegate _xrDestroySpatialAnchorMSFTPtr;

        #endregion

        #region XR_MSFT_spatial_anchor_persistence bindings

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate XrResult CreateSpatialAnchorStoreConnectionMSFTDelegate(ulong session, ref ulong spatialAnchorStore);

        private static CreateSpatialAnchorStoreConnectionMSFTDelegate _xrCreateSpatialAnchorStoreConnectionMSFT;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate XrResult DestroySpatialAnchorStoreConnectionMSFTDelegate(ulong spatialAnchorStore);

        private static DestroySpatialAnchorStoreConnectionMSFTDelegate _xrDestroySpatialAnchorStoreConnectionMSFT;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate XrResult PersistSpatialAnchorMSFTDelegate(ulong spatialAnchorStore, ref XrSpatialAnchorPersistenceInfoMSFT spatialAnchorPersistenceInfo);

        private static PersistSpatialAnchorMSFTDelegate _xrPersistSpatialAnchorMSFT;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate XrResult UnpersistSpatialAnchorMSFTDelegate(ulong spatialAnchorStore, ref XrSpatialAnchorPersistenceNameMSFT spatialAnchorPersistenceName);

        private static UnpersistSpatialAnchorMSFTDelegate _xrUnpersistSpatialAnchorMSFT;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate XrResult EnumeratePersistedSpatialAnchorNamesMSFTDelegate(ulong spatialAnchorStore, uint spatialAnchorNamesCapacityInput, ref uint spatialAnchorNamesCountOutput, IntPtr /*XrSpatialAnchorPersistenceNameMSFT[]*/ spatialAnchorPersistenceNames);

        private static EnumeratePersistedSpatialAnchorNamesMSFTDelegate _xrEnumeratePersistedSpatialAnchorNamesMSFT;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate XrResult CreateSpatialAnchorFromPersistedNameMSFTDelegate(ulong session, ref XrSpatialAnchorFromPersistedAnchorCreateInfoMSFT spatialAnchorCreateInfo, ref ulong spatialAnchor);

        private static CreateSpatialAnchorFromPersistedNameMSFTDelegate _xrCreateSpatialAnchorFromPersistedNameMSFT;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate XrResult ClearSpatialAnchorStoreMSFTDelegate(ulong spatialAnchorStore);

        private static ClearSpatialAnchorStoreMSFTDelegate _xrClearSpatialAnchorStoreMSFT;

        #endregion

        #region OpenXR helper bindings

        [UnmanagedFunctionPointerAttribute(CallingConvention.Cdecl)]
        private delegate XrResult LocateSpaceDelegate(ulong space, ulong baseSpace, long time, ref XrSpaceLocation location);

        private static LocateSpaceDelegate _xrLocateSpacePtr;

        #endregion
    }
}
