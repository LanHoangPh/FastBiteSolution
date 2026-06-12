import * as React from "react";
import { cn } from "@/shared/utils";

export interface SvgIconProps extends React.ComponentPropsWithoutRef<"svg"> {
  /**
   * Size of the icon in pixels (number) or custom string (e.g. "1.5rem" or "100%").
   * Defaults to 20.
   */
  size?: number | string;

  /**
   * Fill color. Defaults to "none".
   */
  fill?: string;

  /**
   * Stroke color. Defaults to "currentColor".
   */
  stroke?: string;

  /**
   * Stroke line width. Defaults to "2".
   */
  strokeWidth?: number | string;

  /**
   * SVG viewBox size. Defaults to "0 0 24 24".
   */
  viewBox?: string;
}

/**
 * A highly reusable SvgIcon wrapper component to import and use custom SVGs easily.
 * It integrates perfectly with Tailwind, Lucide styling conventions, and Light/Dark themes.
 */
export const SvgIcon = React.forwardRef<SVGSVGElement, SvgIconProps>(
  (
    {
      size = 20,
      fill = "none",
      stroke = "currentColor",
      strokeWidth = "2",
      viewBox = "0 0 24 24",
      className,
      children,
      ...props
    },
    ref,
  ) => {
    // If size is a raw number, convert to px. If it's a string, use it directly.
    const sizeStyle = typeof size === "number" ? `${size}px` : size;

    return (
      <svg
        ref={ref}
        width={sizeStyle}
        height={sizeStyle}
        viewBox={viewBox}
        fill={fill}
        stroke={stroke}
        strokeWidth={strokeWidth}
        strokeLinecap="round"
        strokeLinejoin="round"
        className={cn(
          "shrink-0 select-none transition-all duration-200",
          className,
        )}
        {...props}
      >
        {children}
      </svg>
    );
  },
);

SvgIcon.displayName = "SvgIcon";
