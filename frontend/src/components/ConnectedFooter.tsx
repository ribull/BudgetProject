import { Level } from 'react-bulma-components';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import { icon } from '@fortawesome/fontawesome-svg-core/import.macro';
import { CSSProperties } from 'react';

interface ConnectedFooterProps {
  style: CSSProperties;
  isConnected: boolean;
}

export default function ConnectedFooter({
  style,
  isConnected,
}: ConnectedFooterProps) {
  return (
    <Level style={style}>
      <Level.Side align="right">
        <Level.Item>{isConnected ? 'Connected!' : 'Not Connected!'}</Level.Item>
        <Level.Item>
          <FontAwesomeIcon
            color={isConnected ? 'green' : 'red'}
            icon={icon({
              name: isConnected ? 'circle-check' : 'triangle-exclamation',
              family: 'classic',
              style: 'solid',
            })}
          />
        </Level.Item>
      </Level.Side>
    </Level>
  );
}
