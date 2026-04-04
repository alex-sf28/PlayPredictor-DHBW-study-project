import { jwtDecode } from "jwt-decode";

interface JwtPayload {
    exp: number;
}

export function isTokenExpired(token: string): boolean {
    try {
        const decoded = jwtDecode<JwtPayload>(token);
        const now = Math.floor(Date.now() / 1000); // aktuelle Zeit in Sekunden
        return decoded.exp < now;
    } catch (error) {
        return true; // Wenn Token ungültig oder nicht decodierbar ist
    }
}
