import React from 'react';
import { ActivityIndicator, Pressable, ScrollView, StyleSheet, Text, TextInput, TextInputProps, View } from 'react-native';
import { colors, space } from '../theme';

export const Screen = ({
  children,
  centered = false,
}: {
  children: React.ReactNode;
  centered?: boolean;
}) => (
  <ScrollView
    style={s.screen}
    contentContainerStyle={[
      { padding: space.md, gap: space.md },
      centered && { flexGrow: 1, justifyContent: 'center' },
    ]}
    keyboardShouldPersistTaps="handled"
  >
    {children}
  </ScrollView>
);
export const Card = ({ children }: { children: React.ReactNode }) => <View style={s.card}>{children}</View>;
export const H = ({ children }: { children: React.ReactNode }) => <Text style={s.h}>{children}</Text>;
export const Sub = ({ children }: { children: React.ReactNode }) => <Text style={s.sub}>{children}</Text>;
export const Body = ({ children, bold }: { children: React.ReactNode; bold?: boolean }) => <Text style={[s.body, bold && { fontWeight: '700' }]}>{children}</Text>;
export const ErrorText = ({ msg }: { msg?: string | null }) => (msg ? <Text style={s.err}>{msg}</Text> : null);

export function Button({ title, onPress, kind = 'primary', busy, disabled }: { title: string; onPress: () => void; kind?: 'primary' | 'danger' | 'ghost'; busy?: boolean; disabled?: boolean }) {
  const bg = kind === 'primary' ? colors.primary : kind === 'danger' ? colors.danger : 'transparent';
  const fg = kind === 'ghost' ? colors.primary : '#fff';
  return (
    <Pressable accessibilityRole="button" onPress={onPress} disabled={busy || disabled} style={[s.btn, { backgroundColor: bg, opacity: busy || disabled ? 0.5 : 1 }, kind === 'ghost' && { borderWidth: 1, borderColor: colors.primary }]}>
      {busy ? <ActivityIndicator color={fg} /> : <Text style={{ color: fg, fontWeight: '600', fontSize: 16 }}>{title}</Text>}
    </Pressable>
  );
}

export function Field({ label, ...p }: { label: string } & TextInputProps) {
  return (
    <View style={{ gap: space.xs }}>
      <Text style={s.sub}>{label}</Text>
      <TextInput {...p} style={s.input} placeholderTextColor={colors.sub} autoCorrect={false} />
    </View>
  );
}

export function Chips<T extends string>({ options, value, onChange }: { options: { value: T; label: string }[]; value: T | null; onChange: (v: T) => void }) {
  return (
    <View style={{ flexDirection: 'row', flexWrap: 'wrap', gap: space.sm }}>
      {options.map(o => (
        <Pressable key={o.value} accessibilityRole="button" onPress={() => onChange(o.value)} style={[s.chip, value === o.value && { backgroundColor: colors.primary }]}>
          <Text style={{ color: value === o.value ? '#fff' : colors.text }}>{o.label}</Text>
        </Pressable>
      ))}
    </View>
  );
}

const s = StyleSheet.create({
  screen: { flex: 1, backgroundColor: colors.bg },
  card: { backgroundColor: colors.card, borderRadius: 12, padding: space.md, gap: space.sm, borderWidth: StyleSheet.hairlineWidth, borderColor: colors.border },
  h: { fontSize: 20, fontWeight: '700', color: colors.text },
  sub: { fontSize: 13, color: colors.sub },
  body: { fontSize: 16, color: colors.text },
  err: { color: colors.danger, fontSize: 14 },
  btn: { minHeight: 48, borderRadius: 10, alignItems: 'center', justifyContent: 'center', paddingHorizontal: space.md },
  input: { minHeight: 48, borderWidth: 1, borderColor: colors.border, borderRadius: 10, paddingHorizontal: space.md, backgroundColor: '#fff', color: colors.text, fontSize: 16 },
  chip: { paddingHorizontal: space.md, paddingVertical: 10, borderRadius: 20, backgroundColor: colors.chip },
});
