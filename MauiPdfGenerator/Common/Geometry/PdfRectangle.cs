using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MauiPdfGenerator.Common.Geometry;

/// <summary>
/// Represents an immutable rectangle defined by location (X, Y) and size (Width, Height).
/// Width and Height must be non-negative.
/// Marked internal as it's primarily for internal library use.
/// </summary>
internal readonly struct PdfRectangle : IEquatable<PdfRectangle>
{
    /// <summary>
    /// Gets a rectangle with zero location and size.
    /// </summary>
    public static readonly PdfRectangle Zero = new PdfRectangle(0, 0, 0, 0);

    /// <summary>
    /// Gets the X-coordinate of the top-left corner.
    /// </summary>
    public double X { get; }

    /// <summary>
    /// Gets the Y-coordinate of the top-left corner.
    /// </summary>
    public double Y { get; }

    /// <summary>
    /// Gets the width of the rectangle.
    /// </summary>
    public double Width { get; }

    /// <summary>
    /// Gets the height of the rectangle.
    /// </summary>
    public double Height { get; }

    // --- Calculated Properties ---

    /// <summary>
    /// Gets the X-coordinate of the left edge (equivalent to X).
    /// </summary>
    public double Left => X;

    /// <summary>
    /// Gets the Y-coordinate of the top edge (equivalent to Y).
    /// </summary>
    public double Top => Y;

    /// <summary>
    /// Gets the X-coordinate of the right edge.
    /// </summary>
    public double Right => X + Width;

    /// <summary>
    /// Gets the Y-coordinate of the bottom edge.
    /// </summary>
    public double Bottom => Y + Height;

    /// <summary>
    /// Gets the location of the top-left corner as a PdfPoint.
    /// </summary>
    public PdfPoint Location => new PdfPoint(X, Y);

    /// <summary>
    /// Gets the size of the rectangle as a PdfSize.
    /// </summary>
    public PdfSize Size => new PdfSize(Width, Height);

    /// <summary>
    /// Gets the center point of the rectangle.
    /// </summary>
    public PdfPoint Center => new PdfPoint(X + Width / 2.0, Y + Height / 2.0);

    /// <summary>
    /// Gets a value indicating whether the rectangle has zero size.
    /// </summary>
    public bool IsEmpty => Width <= 0 || Height <= 0; // Use <= 0 consistent with some frameworks

    // --- Constructors ---

    /// <summary>
    /// Initializes a new instance of the <see cref="PdfRectangle"/> struct.
    /// </summary>
    /// <param name="x">The X-coordinate of the top-left corner.</param>
    /// <param name="y">The Y-coordinate of the top-left corner.</param>
    /// <param name="width">The width (must be non-negative).</param>
    /// <param name="height">The height (must be non-negative).</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if width or height is negative.</exception>
    public PdfRectangle(double x, double y, double width, double height)
    {
        if (width < 0)
            throw new ArgumentOutOfRangeException(nameof(width), "Width cannot be negative.");
        if (height < 0)
            throw new ArgumentOutOfRangeException(nameof(height), "Height cannot be negative.");

        X = x;
        Y = y;
        Width = width;
        Height = height;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PdfRectangle"/> struct.
    /// </summary>
    /// <param name="location">The top-left corner position.</param>
    /// <param name="size">The size (width and height).</param>
    public PdfRectangle(PdfPoint location, PdfSize size)
        : this(location.X, location.Y, size.Width, size.Height)
    {
    }

    // --- Utility Methods ---

    /// <summary>
    /// Determines whether the specified point is contained within this rectangle.
    /// </summary>
    /// <param name="point">The point to test.</param>
    /// <returns>True if the point is within the rectangle; otherwise, false.</returns>
    public bool Contains(PdfPoint point)
    {
        return point.X >= Left && point.X < Right && // Use < Right/Bottom for consistency
               point.Y >= Top && point.Y < Bottom;
    }

    /// <summary>
    /// Determines whether the specified rectangle is entirely contained within this rectangle.
    /// </summary>
    /// <param name="rect">The rectangle to test.</param>
    /// <returns>True if the rectangle is entirely within this rectangle; otherwise, false.</returns>
    public bool Contains(PdfRectangle rect)
    {
        return rect.Left >= Left && rect.Right <= Right &&
               rect.Top >= Top && rect.Bottom <= Bottom;
    }

    /// <summary>
    /// Determines whether this rectangle intersects with another rectangle.
    /// </summary>
    /// <param name="rect">The rectangle to test.</param>
    /// <returns>True if the rectangles intersect; otherwise, false.</returns>
    public bool IntersectsWith(PdfRectangle rect)
    {
        return rect.Left < Right && Left < rect.Right &&
               rect.Top < Bottom && Top < rect.Bottom;
    }

    /// <summary>
    /// Creates a new rectangle inflated by the specified horizontal and vertical amounts.
    /// Negative values will deflate the rectangle.
    /// </summary>
    /// <param name="x">The amount to inflate horizontally (applied to both left and right).</param>
    /// <param name="y">The amount to inflate vertically (applied to both top and bottom).</param>
    /// <returns>A new, inflated rectangle.</returns>
    public PdfRectangle Inflate(double x, double y)
    {
        double newX = X - x;
        double newY = Y - y;
        double newWidth = Width + 2 * x;
        double newHeight = Height + 2 * y;

        // Ensure width/height don't become negative due to deflation
        if (newWidth < 0) newWidth = 0;
        if (newHeight < 0) newHeight = 0;


        return new PdfRectangle(newX, newY, newWidth, newHeight);
    }

    /// <summary>
    /// Creates a new rectangle offset by the specified horizontal and vertical amounts.
    /// </summary>
    /// <param name="x">The horizontal offset.</param>
    /// <param name="y">The vertical offset.</param>
    /// <returns>A new, offset rectangle.</returns>
    public PdfRectangle Offset(double x, double y)
    {
        return new PdfRectangle(X + x, Y + y, Width, Height);
    }

    /// <summary>
    /// Creates a new rectangle offset by the specified point.
    /// </summary>
    /// <param name="point">The offset point.</param>
    /// <returns>A new, offset rectangle.</returns>
    public PdfRectangle Offset(PdfPoint point)
    {
        return Offset(point.X, point.Y);
    }


    // --- Equality Methods ---

    public bool Equals(PdfRectangle other)
    {
        // See comment in PdfPoint regarding tolerance if needed
        return X.Equals(other.X) && Y.Equals(other.Y) &&
               Width.Equals(other.Width) && Height.Equals(other.Height);
    }

    public override bool Equals(object? obj)
    {
        return obj is PdfRectangle other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(X, Y, Width, Height);
    }

    // --- Operators ---

    public static bool operator ==(PdfRectangle left, PdfRectangle right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(PdfRectangle left, PdfRectangle right)
    {
        return !(left == right);
    }

    // --- ToString ---

    [ExcludeFromCodeCoverage]
    public override string ToString()
    {
        return $"{{X={X}, Y={Y}, Width={Width}, Height={Height}}}";
    }
}
