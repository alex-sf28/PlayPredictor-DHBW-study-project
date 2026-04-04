import { OptionMenuItemList } from "@/types/navigation";

export const menuOptionsLoggedIn: OptionMenuItemList = [{
    displayName: "Settings",
    value: "settings",
    action: "/settings"
},
{
    displayName: "Logout",
    value: "logout",
    action: "/auth/logout"
}]

export const menuOptionsLoggedOut: OptionMenuItemList = [{
    displayName: "Login",
    value: "login",
    action: "/auth/login"
},
{
    displayName: "Register",
    value: "register",
    action: "/auth/register"
}]