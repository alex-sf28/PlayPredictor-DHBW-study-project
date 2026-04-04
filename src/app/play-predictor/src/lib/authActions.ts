// src/lib/authActions.ts
import { getApiUsersMe, LoginResponse } from "@/client";
import { useAuth } from "./authStore";
import { client } from "@/client/client.gen";


const BASE_URL = process.env.NEXT_PUBLIC_API_URL;

export async function initializeAuth() {
    const { setAccessToken, setUser, setIsLoading, logout } = useAuth.getState();

    try {
        // Versuch, neuen Access Token über Refresh Cookie zu bekommen
        const refreshRes = await fetch(`${BASE_URL}/api/Auth/refresh`, {
            method: "POST",
            credentials: "include",
        });

        if (refreshRes.ok) {
            const result = (await refreshRes.json()) as LoginResponse;

            if (result.token) {
                console.log("Setting new access token")
                setAccessToken(result.token);
                setUser(result.user)
            }
        } else {

        }
    } catch (err) {
        console.error("Auth initialization failed:", err);

    } finally {
        setIsLoading(false);
    }
}
