import { type ClassValue, clsx } from "clsx";
import { twMerge } from "tailwind-merge";

export function cn(...inputs: ClassValue[]) {
  return twMerge(clsx(inputs));
}

export type OptionalExcept<T, K extends keyof T> = Partial<T> & Pick<T, K>;
