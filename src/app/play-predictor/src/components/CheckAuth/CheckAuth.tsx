"use client"

import { initializeAuth } from "@/lib/authActions";
import { useAuth } from "@/lib/authStore";
import { Spinner } from "@chakra-ui/react";
import { usePathname } from "next/navigation";
import { useEffect } from "react";

export default function CheckAuth({ children }: { children: React.ReactNode }) {
    const pathname = usePathname()
    const { setIsLoading, isLoading } = useAuth()

    useEffect(() => {
        if (!pathname.includes("/login") && !pathname.includes("/register") && !pathname.includes("/logout")) {
            initializeAuth();
        } else {
            setIsLoading(false)
        }
    }, [])

    if (isLoading) {
        return <Spinner
            size="lg"
            color="brand"
            position="absolute"
            top="50%"
            left="50%"
            translate="-50% -50%"
        />;
    }

    return (
        <>
            {children}
        </>
    )
}
