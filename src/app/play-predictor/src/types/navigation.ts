export type NavTabField = {
    displayName: string | React.ReactNode;
    pathName: string;
};

export type NavTabProps = {
    navBarFields: NavTabField[];
};

export type OptionMenu = {
    children?: React.ReactNode
    itemList: OptionMenuItemList
}

export type OptionMenuItemList = {
    displayName: string | React.ReactNode
    value: string
    action: string
}[]

export type SideWaysTabsSection = {
    title?: string | React.ReactNode,
    items: NavTabField[]
}

export type SideWaysTabsProps = {
    sideTabFields: SideWaysTabsSection[]
}
