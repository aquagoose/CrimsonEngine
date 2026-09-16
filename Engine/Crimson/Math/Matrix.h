#pragma once

#include "Coredefs.h"
#include "Vec4.h"

namespace cge
{
    /**
     * Represents a 4x4 matrix.
     * @tparam T A numeric type.
     */
    template<typename T>
    struct Matrix
    {
        Vec4<T> Row0; // The first row.
        Vec4<T> Row1; // The second row.
        Vec4<T> Row2; // The third row.
        Vec4<T> Row3; // The fourth row.

        Matrix(Vec4<T> row0, Vec4<T> row1, Vec4<T> row2, Vec4<T> row3) :
            Row0(row0), Row1(row1), Row2(row2), Row3(row3) {}

        Matrix(T m00, T m01, T m02, T m03,
               T m10, T m11, T m12, T m13,
               T m20, T m21, T m22, T m23,
               T m30, T m31, T m32, T m33)
        {
            Row0 = { m00, m01, m02, m03 };
            Row1 = { m10, m11, m12, m13 };
            Row2 = { m20, m21, m22, m23 };
            Row3 = { m30, m31, m32, m33 };
        }

        Matrix()
        {
            Row0 = {};
            Row1 = {};
            Row2 = {};
            Row3 = {};
        }

        Vec4<T> Column0() const
        {
            return { Row0.X, Row1.X, Row2.X, Row3.X };
        }

        Vec4<T> Column1() const
        {
            return { Row0.Y, Row1.Y, Row2.Y, Row3.Y };
        }

        Vec4<T> Column2() const
        {
            return { Row0.Z, Row1.Z, Row2.Z, Row3.Z };
        }

        Vec4<T> Column3() const
        {
            return { Row0.W, Row1.W, Row2.W, Row3.W };
        }

        friend Matrix operator *(const Matrix& left, const Matrix& right)
        {
            auto row0 = left.Row0;
            auto row1 = left.Row1;
            auto row2 = left.Row2;
            auto row3 = left.Row3;

            auto col0 = right.Column0();
            auto col1 = right.Column1();
            auto col2 = right.Column2();
            auto col3 = right.Column3();

            return {
                { Vec4<T>::Dot(row0, col0), Vec4<T>::Dot(row0, col1), Vec4<T>::Dot(row0, col2), Vec4<T>::Dot(row0, col3) },
                { Vec4<T>::Dot(row1, col0), Vec4<T>::Dot(row1, col1), Vec4<T>::Dot(row1, col2), Vec4<T>::Dot(row1, col3) },
                { Vec4<T>::Dot(row2, col0), Vec4<T>::Dot(row2, col1), Vec4<T>::Dot(row2, col2), Vec4<T>::Dot(row2, col3) },
                { Vec4<T>::Dot(row3, col0), Vec4<T>::Dot(row3, col1), Vec4<T>::Dot(row3, col2), Vec4<T>::Dot(row3, col3) },
            };
        }

        static Matrix Identity()
        {
            return {
                Vec4<T>::UnitX(),
                Vec4<T>::UnitY(),
                Vec4<T>::UnitZ(),
                Vec4<T>::UnitW(),
            };
        }

        static Matrix Orthographic(T left, T right, T bottom, T top, T near, T far)
        {
            T rightPlusLeft = right + left;
            T topPlusBottom = top + bottom;
            T farPlusNear = far + near;

            T rightMinusLeft = right - left;
            T topMinusBottom = top - bottom;
            T farMinusNear = far - near;

            return {
                Vec4<T>(2.0 / rightMinusLeft, 0, 0, 0),
                Vec4<T>(0, 2.0 / topMinusBottom, 0, 0),
                Vec4<T>(0, 0, -2.0 / farMinusNear, 0),
                Vec4<T>(-(rightPlusLeft / rightMinusLeft), -(topPlusBottom / topMinusBottom), -(farPlusNear / farMinusNear), 1.0)
            };
        }
    };

    using Matrixi = Matrix<i32>;
    using Matrixf = Matrix<f32>;
    using Matrixd = Matrix<f64>;
}