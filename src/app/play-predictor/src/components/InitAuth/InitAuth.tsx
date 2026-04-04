"use client"

import { useAuth } from "@/lib/authStore";
import { client } from "../../client/client.gen"

const BASE_URL = process.env.NEXT_PUBLIC_API_URL ?? "https://localhost:7011"

export default function InitAuth({ children }: { children: React.ReactNode }) {
    const { accessToken } = useAuth();

    client.setConfig({
        credentials: "include",
        baseUrl: BASE_URL,
        auth: () => accessToken ? `Bearer ${accessToken}` : undefined,
    });

    return <>{children}</>;

}