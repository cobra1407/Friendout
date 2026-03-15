import { clsx, type ClassValue } from "clsx"
import { twMerge } from "tailwind-merge"

/**
 * A utility function that merges multiple class names into a single string.
 * It uses `clsx` under the hood to handle the merging of class names.
 *
 * @param {...inputs} - A variable number of class names to merge.
 *
 * @returns The merged class names as a single string.
 */
export function cn(...inputs: ClassValue[]) {
  return twMerge(clsx(inputs))
}
