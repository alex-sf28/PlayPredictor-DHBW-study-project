import { Separator, Skeleton, SkeletonText, Stack } from "@chakra-ui/react";

export default function SettingsSkeleton() {
    return (
        <Stack gap={6}>
            <Stack>
                <Skeleton height="8" width={400} />
                <Separator />
                <SkeletonText noOfLines={3} />
            </Stack>
            <Stack>
                <Skeleton height="8" width={400} />
                <Separator />
                <SkeletonText noOfLines={3} />
            </Stack>
        </Stack>
    );
}