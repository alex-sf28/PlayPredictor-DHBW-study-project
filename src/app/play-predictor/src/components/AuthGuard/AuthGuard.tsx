"use client";

import { useAuth } from "@/lib/authStore";
import { usePathname, useRouter } from "next/navigation";
import { useEffect } from "react";

export default function AuthGuard({ children }: { children: React.ReactNode }) {
    const { accessToken, setRedirectUri } = useAuth();
    const router = useRouter();
    const pathname = usePathname();

    useEffect(() => {
        if (!accessToken) {
            router.replace("/auth/login")
            setRedirectUri(pathname)
        }
    }, [accessToken, router]);

    if (!accessToken) return null;
    return <>{children}</>;
}
