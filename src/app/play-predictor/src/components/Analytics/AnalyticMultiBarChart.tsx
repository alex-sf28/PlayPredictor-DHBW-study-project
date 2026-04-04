import { filterRecordValues, prettyLabeling } from '@/lib/helpers';
import { Box, createListCollection, Heading, Portal, Select } from '@chakra-ui/react';
import { useState } from 'react';
import { BarChart, Legend, XAxis, YAxis, CartesianGrid, Tooltip, Bar } from 'recharts';

interface AnalyticBarChartProps {
    name: string;
    description: string;
    chartData: {
        [key: string]: {
            [metric: string]: number | string | null;
        };
    };
    includeOnly?: string[];
    preSelected?: string[];
}

const colors = [
    "var(--chakra-colors-brand-solid)",
    '#ffc658',
    '#ff7300',
    '#00ff00'
];

export default function AnalyticMultiBarChart({
    name,
    description,
    chartData,
    includeOnly,
    preSelected
}: AnalyticBarChartProps) {



    const data = Object.entries(includeOnly ? filterRecordValues(chartData, includeOnly) : chartData).map(([key, value]) => ({
        name: key,
        ...Object.fromEntries(
            Object.entries(value).map(([k, v]) => [
                k,
                v !== null ? Math.round(Number(v) * 10000) / 10000 : null
            ])
        )
    }));

    // automatisch alle Keys holen (Kills, Deaths, ...)
    const dataKeys = Object.keys(data[0] || {}).filter(k => k !== 'name');

    const [selected, setSelected] = useState<string[]>(preSelected ?? dataKeys);

    function updateSelected(e: string[]) {
        setSelected(e);
    }

    const collection = createListCollection({ items: dataKeys.map((value) => ({ label: prettyLabeling(value), value: value })) })

    return (
        <Box>
            <Heading size="md" mb={2}>{name}</Heading>
            <Heading size="sm" mb={4} color="gray.500">{description}</Heading>

            <Select.Root
                multiple
                collection={
                    collection
                }
                width="320px"
                value={selected}
                onValueChange={(e) => updateSelected(e.value)}
                margin={5}
            >
                <Select.HiddenSelect />
                <Select.Label>Select framework</Select.Label>
                <Select.Control>
                    <Select.Trigger>
                        <Select.ValueText placeholder="Select framework" />
                    </Select.Trigger>
                    <Select.IndicatorGroup>
                        <Select.Indicator />
                    </Select.IndicatorGroup>
                </Select.Control>
                <Portal>
                    <Select.Positioner>
                        <Select.Content>
                            {collection.items.map((metric) => (
                                <Select.Item item={metric} key={metric.value}>
                                    {metric.label}
                                    <Select.ItemIndicator />
                                </Select.Item>
                            ))}
                        </Select.Content>
                    </Select.Positioner>
                </Portal>
            </Select.Root>

            <BarChart
                style={{ width: '100%', maxWidth: '700px', maxHeight: '70vh', aspectRatio: 1.618 }}
                data={data}
                barGap={0}
                responsive
            >
                <CartesianGrid strokeDasharray="3 3" />
                <XAxis dataKey="name" />
                <YAxis />
                <Tooltip />
                <Legend />

                {selected.map((key, index) => (
                    <Bar
                        key={key}
                        dataKey={key}
                        fill={colors[index % colors.length]}
                    />
                ))}
            </BarChart>
        </Box>
    );
}