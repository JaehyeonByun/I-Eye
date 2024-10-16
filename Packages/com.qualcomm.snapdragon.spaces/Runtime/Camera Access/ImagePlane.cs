/****************************************************************
 * File: ImagePlane.cs
 * Copyright (c) 2024 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
 *
 * Confidential and Proprietary - Qualcomm Technologies, Inc.
 ****************************************************************/

namespace Qualcomm.Snapdragon.Spaces
{
    internal class ImagePlane
    {
        public uint Root => _root;
        public uint Stride => _stride;

        private uint _root;
        private uint _stride;
        private uint _offset;
        private uint _step;
        private uint _columnRate;
        private uint _rowRate;

        public ImagePlane(uint root, uint stride, uint offset, uint step, uint columnRate, uint rowRate)
        {
            _root = root;
            _stride = stride;
            _offset = offset;
            _step = step;
            _columnRate = columnRate;
            _rowRate = rowRate;
        }

        // ImagePlane is an abstraction of Y, U or V plane to sample the frameBuffer correctly, given Row and Column values.
        // ImagePlane is defined by: Root, Stride, Offset, Step, ColumnRate and RowRate
        // Root:    Index where plane data begins
        // Stride:  Row size in bytes
        // Offset:  Position of the correct byte inside a Y, UV or YUYV byte group
        // Step:    Length of the byte group, Y(1), UV(2), YUYV(4)
        // ColumnRate:  Pixels represented by each byte group, horizontally.
        // RowRate:     Pixels represented by each byte group, vertically.
        //
        // For more information: https://en.wikipedia.org/wiki/YCbCr#Packed_pixel_formats_and_conversion

        public int GetOffset(int column, int row)
        {
            var rowOffset = (row / _rowRate * _stride);
            var columnOffset = (column / _columnRate * _step + _offset);

            return (int) (_root + rowOffset + columnOffset);
        }
    }
}
