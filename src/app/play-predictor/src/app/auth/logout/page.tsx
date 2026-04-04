"use client"

import { useAuth } from "@/lib/authStore";
import { SuccessMessage } from "@/lib/message";
import { Center, Spinner } from "@chakra-ui/react";
import { useRouter } from "next/navigation";
import { useEffect } from "react";

export default function logoutUser() {

    const router = useRouter();
    const { logout } = useAuth.getState()

    useEffect(() => {
        SuccessMessage("Logging out...", "You will be returned to the Login Page.");
        logout()
        router.replace("/auth/login")
    }, [router]);

    return <Center><Spinner color="primary" size="lg"></Spinner></Center>
}