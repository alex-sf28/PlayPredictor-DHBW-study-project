// app/hooks/useActiveTab.ts
"use client";

import { usePathname } from "next/navigation";
import { useMemo } from "react";

export default function useActiveTab() {
    const pathname = usePathname();

    return useMemo(
        () => (path: string) => pathname?.startsWith(path.startsWith("/") ? path : `/${path}`),
        [pathname]
    );
}
