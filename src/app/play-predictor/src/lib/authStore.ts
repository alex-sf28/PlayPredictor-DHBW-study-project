// lib/authStore.ts
import { LoginGoogleRequest, LoginRequest, LoginResponse, postApiAuthGoogle, postApiAuthLogin, postApiAuthLogout, ProblemDetails, User } from "@/client";
import { create } from "zustand";

type AuthState = {
    accessToken: string | null;
    user: User | undefined;
    setAccessToken: (token: string | null) => void;
    setUser: (user: User | undefined) => void;
    logout: () => Promise<void>;
    login: (data: LoginRequest) => Promise<LoginResponse | ProblemDetails>
    loginWithGoogle: (data: LoginGoogleRequest) => Promise<LoginResponse | ProblemDetails>
    isLoading: boolean,
    setIsLoading: (loading: boolean) => void
    redirectUri: string | null
    setRedirectUri: (uri: string) => void
};

export const useAuth = create<AuthState>((set) => ({
    accessToken: null,
    user: undefined,
    isLoading: true,
    redirectUri: null,
    setAccessToken: (token) => set({ accessToken: token }),
    setUser: (user) => set({ user }),
    async logout() {
        await postApiAuthLogout();
        set({ accessToken: null, user: undefined })
    },
    async login(data) {

        const res = await postApiAuthLogin({
            body: {
                email: data.email, password: data.password
            }
        });

        if (res.data?.token) {
            set({ accessToken: res.data.token, user: res.data.user })
            return res.data
        } else if (res.error) {
            return res.error
        }
        return { title: "Error" }

    },
    async loginWithGoogle(data) {
        const res = await postApiAuthGoogle({
            body: {
                idToken: data.idToken
            }
        })

        if (res.data?.token) {
            set({ accessToken: res.data.token, user: res.data.user })
            return res.data
        } else if (res.error) {
            return res.error
        }
        return { title: "Error" }
    },
    setIsLoading: (loading) => set({ isLoading: loading }),
    setRedirectUri: (uri) => set({ redirectUri: uri })
}));
